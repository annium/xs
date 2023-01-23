create table main.metapackages (
	id uuid not null,
	type text not null,
	name text not null,
	version text not null,
	description text not null,
	published timestamptz not null,
	downloads int not null,
	owner_id uuid not null,
	constraint pk_metapackages primary key (id),
	constraint fk_metapackages_apps_app_id foreign key (app_id) references main.apps(id) on delete restrict
);
create unique index ix_metapackages_type_name_version on main.metapackages using btree (type, name, version);