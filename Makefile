# Currently hardcodes to x64, later can try these answers to detect ARM:
# https://stackoverflow.com/questions/4058840/makefile-that-distinguishes-between-windows-and-unix-like-systems

build:
	dotnet publish --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=embedded --output publish/
