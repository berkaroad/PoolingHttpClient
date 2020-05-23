all: pack

test:
	dotnet test `pwd`/PoolingHttpClient/PoolingHttpClient.Tests

pack: build
	mkdir -p `pwd`/packages
	dotnet pack -c Release `pwd`/PoolingHttpClient/PoolingHttpClient/
	mv `pwd`/PoolingHttpClient/PoolingHttpClient/bin/Release/*.nupkg `pwd`/packages/

build:
	dotnet build -c Release `pwd`/PoolingHttpClient/PoolingHttpClient/
