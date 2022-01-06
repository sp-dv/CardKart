rm -rf bin
rm -rf obj
rm -rf rls

dotnet restore
dotnet msbuild

cp -r bin/Debug/netcoreapp3.1 rls

rm -rf bin
rm -rf obj
