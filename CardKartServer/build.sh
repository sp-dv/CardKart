rm -rf bin
rm -rf obj
rm -rf rls

dotnet restore
dotnet msbuild

cp -r bin/Debug/net5.0 rls

rm -rf bin
rm -rf obj
