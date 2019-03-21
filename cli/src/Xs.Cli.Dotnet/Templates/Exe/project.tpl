<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <PackageId>{{name}}</PackageId>
        <PackageVersion>0.1.0</PackageVersion>
        <Description>{{name}}</Description>
        <TargetFramework>netcoreapp2.2</TargetFramework>
        <OutputType>Exe</OutputType>
        <DebugType>portable</DebugType>
        <WarningsAsErrors>true</WarningsAsErrors>
    </PropertyGroup>
    <ItemGroup>
        <PackageReference Include="Annium.Extensions.DependencyInjection" Version="0.1.0" />
        <PackageReference Include="Annium.Extensions.Entrypoint" Version="0.1.0" />
    </ItemGroup>
</Project>