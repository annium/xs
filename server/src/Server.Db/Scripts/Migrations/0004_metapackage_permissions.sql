create table main.metapackage_permissions (
	metapackage_id uuid not null,
	category int not null,
	permission int not null,
	constraint pk_metapackage_permissions primary key (metapackage_id),
	constraint fk_metapackage_permissions_metapackages_metapackage_id foreign key (metapackage_id) references main.metapackages(id) on delete restrict
);