create table main.meta_packages (
	id uuid not null,
	type text not null,
	name text not null,
	version text not null,
	description text not null,
	published timestamptz not null,
	downloads int not null,
	owner_id uuid not null,
	constraint pk_meta_packages primary key (id),
	constraint fk_meta_packages_users_owner_id foreign key (owner_id) references main.users(id) on delete restrict
);
create unique index ix_meta_packages_type_name_version on main.meta_packages using btree (type, name, version);