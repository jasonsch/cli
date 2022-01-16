@echo off
mkdir temp_binaries

copy /y b64dec\bin\Release\netcoreapp3.0\b64dec.dll temp_binaries
copy /y b64enc\bin\Release\netcoreapp3.0\b64enc.dll temp_binaries
copy /y datpass\bin\Release\netcoreapp3.0\datpass.dll temp_binaries
copy /y datpass\bin\Release\netcoreapp3.0\Newtonsoft.Json.dll temp_binaries
copy /y diskusage\bin\Release\netcoreapp2.1\diskusage.dll temp_binaries
copy /y filecount\bin\Release\netcoreapp2.2\filecount.dll temp_binaries
copy /y find\bin\Release\netcoreapp2.2\find.exe temp_binaries
copy /y flatten\bin\Release\netcoreapp3.0\flatten.dll temp_binaries
copy /y gcal\bin\Release\netcoreapp2.1\gcal.dll temp_binaries
copy /y grep\bin\Release\netcoreapp3.0\grep.dll temp_binaries
copy /y grep\bin\Release\netcoreapp3.0\Mono.Options.dll temp_binaries
copy /y mv\bin\Release\netcoreapp3.0\mv.dll temp_binaries
copy /y sleep\bin\Release\netcoreapp3.0\sleep.dll temp_binaries
copy /y uniq\bin\Release\netcoreapp2.1\uniq.dll temp_binaries
copy /y urldec\bin\Release\netcoreapp3.0\urldec.dll temp_binaries
copy /y urlenc\bin\Release\netcoreapp3.0\urlenc.dll temp_binaries
copy /y uuid\bin\Release\netcoreapp3.0\uuid.dll temp_binaries
copy /y yas\bin\Release\netcoreapp2.1\yas.dll temp_binaries

del /q cli.binaries.zip
powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('temp_binaries', 'cli.binaries.zip'); }"

rd /s /q temp_binaries
