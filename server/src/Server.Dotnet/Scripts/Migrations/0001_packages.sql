create table dotnet.packages (
	id uuid not null,
	meta_package_id uuid not null,
	name text not null,
	version text not null,
	description text not null,
	published timestamptz not null,
	downloads int not null,
	constraint pk_packages primary key (id),
	constraint fk_packages_meta_packages_meta_package_id foreign key (meta_package_id) references main.meta_packages(id) on delete restrict
);
create unique index ix_packages_type_name_version on dotnet.packages using btree (type, name, version);