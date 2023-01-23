create table dotnet.package_dependencies (
	package_id uuid not null,
	framework text not null,
	name text not null,
	version text not null,
	constraint pk_package_dependencies primary key (package_id, framework, name, version),
	constraint fk_package_dependencies_packages_package_id foreign key (package_id) references dotnet.packages(id) on delete restrict
);