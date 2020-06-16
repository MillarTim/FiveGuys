@echo Running Code Generator

copy "%ProgramFiles(x86)%\CSS\BusinessServices\v2.0\Schemas\xsdcodegenerator.exe" 
copy "%ProgramFiles(x86)%\CSS\BusinessServices\v2.0\Schemas\xsdcodegenerator.exe.config" 

xsdCodeGenerator FileTypeMapConfig.xsd "" .\ /all /multi
xsdCodeGenerator FileProcessorConfig.xsd "" .\ /all /multi