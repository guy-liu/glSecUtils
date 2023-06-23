
function FindUnsafeMethod ($MethodName) {
    
    $assemblies = [AppDomain]::CurrentDomain.GetAssemblies()

    # Go through all assemblies/modules
    foreach ($assembly in $assemblies) {
    
        $unsafeTypes = $assembly.getTypes() | Where-Object { $_.Name.Contains("Unsafe") }

        # Go through all types/classes
        foreach ($unsafeType in $unsafeTypes) {

            $staticMembers = $unsafeType | Get-Member -Static 2>$null
        
            foreach ($staticMember in $staticMembers) {
                
                if ( $staticMember.Name.Contains($MethodName) ) {
                    
                    $methods = $unsafeType.GetMethods() | Where-Object { $_.Name -eq $MethodName }

                    return $methods[0]
                }
            }
        }
    }

    return $null
}


$getModuleHandleMethod = FindUnsafeMethod("GetModuleHandle")
$getProcAddressMethod = FindUnsafeMethod("GetProcAddress")

$kernel32 = $getModuleHandleMethod.Invoke($null, @("kernel32.dll"))
$winExec = $getProcAddressMethod.Invoke($null, @($kernel32, "WinExec"))

$MyAssembly = New-Object System.Reflection.AssemblyName('ReflectedDelegate')
$Domain = [AppDomain]::CurrentDomain
$MyAssemblyBuilder = $Domain.DefineDynamicAssembly($MyAssembly, [System.Reflection.Emit.AssemblyBuilderAccess]::Run)
$MyModuleBuilder = $MyAssemblyBuilder.DefineDynamicModule('InMemoryModule', $false)
$MyTypeBuilder = $MyModuleBuilder.DefineType('MyDelegateType', 'Class, Public, Sealed, AnsiClass, AutoClass', [System.MulticastDelegate])

$MyConstructorBuilder = $MyTypeBuilder.DefineConstructor('RTSpecialName, HideBySig, Public', 
    [System.Reflection.CallingConventions]::Standard, 
    @([String], [int]))
$MyConstructorBuilder.SetImplementationFlags('Runtime, Managed')
$MyMethodBuilder = $MyTypeBuilder.DefineMethod('Invoke', 
    'Public, HideBySig, NewSlot, Virtual', 
    [int], @([String], [int]))
$MyMethodBuilder.SetImplementationFlags('Runtime, Managed')
$MyDelegateType = $MyTypeBuilder.CreateType()

$MyFunction = [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer($winExec, $MyDelegateType)
$MyFunction.Invoke("notepad.exe",1)

