echo Build IFC installer

echo %1
echo %2

set ThisBatFileRoot=%~dp0
rem Set this path to your Wix bin directory.
set WixRoot=%ThisBatFileRoot%..\..\..\..\..\..\ThirdParty\Wix\v37\

rem It is necessary to add the Wix bin directory to the system path temporarily to use the -ext flag below.
SET PATH=%PATH%;%WixRoot%

candle.exe -dProjectDir=%2 -ext WixUtilExtension %2Product.wxs 
light.exe -ext WixUtilExtension -out RevitIFC2018.msi product.wixobj -ext WixUIExtension

copy RevitIFC2018.msi %1..\Releasex64
del RevitIFC2018.msi

echo %1..\Releasex64\RevitIFC2018.msi
