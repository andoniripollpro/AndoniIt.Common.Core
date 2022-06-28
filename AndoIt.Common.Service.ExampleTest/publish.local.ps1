
$ServiceName = "AndoIt.Common.ExampleTest.Local"
$originPath = "bin\Debug"
$ProgramPublishLocation = "C:\DesarrolloAps\" + $ServiceName
"ServiceName = " + $ServiceName
"originPath = " + $originPath
"ProgramPublishLocation = " + $ProgramPublishLocation 
Get-Location

"Compilando: Resultados en compillation.output"
&'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe' .\AndoIt.Common.Service.ExampleTest.csproj /t:Clean /t:Rebuild /p:Configuration=Debug > compillation.output
&'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe' .\AndoIt.Common.Service.ExampleTest.csproj /t:Build /p:Configuration=Debug

"Va a detener el servicio si tiene acceso a él"
$ServicioInstalado = Get-Service $ServiceName
if ($ServicioInstalado)
{
	"Parando servicio"
	Stop-Service -Name $ServiceName -Force
	"Intenta matar el proceso por si acaso. Si no existe que es lo normal saldrá error:"
	taskkill /F /IM "AndoIt.Common.Service.exe" /T
} else {
	"No existe servicio instalado con ese nombre. Hay que instalarlo. No hagas caso del ROJO"
}

"Limpiando ficheros de otros deploys"
Mkdir -Force $ProgramPublishLocation
Remove-Item $ProgramPublishLocation\* -Force -Recurse

Get-ChildItem -Path $originPath | Foreach-object { write-output $_.FullName; Copy-item -path $_.FullName -Destination $ProgramPublishLocation -verbose -recurse -Force }
#No uso el transform que es una lata
Copy-item App.config $ProgramPublishLocation\AndoIt.Common.Service.exe.config
Set-Location $ProgramPublishLocation

if ($ServicioInstalado)
{
	"Arrancando el servicio, o no..."
	Start-Service -Name $ServiceName	
} else {
	"Instalando"
	cmd /c "install.cmd"
}

"Pulsa una tecla para cerrar"
cmd /c pause | out-null
