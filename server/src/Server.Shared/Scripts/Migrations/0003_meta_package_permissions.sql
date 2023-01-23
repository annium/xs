create table main.meta_package_permissions (
	meta_package_id uuid not null,
	category int not null,
	permission int not null,
	constraint pk_meta_package_permissions primary key (meta_package_id),
	constraint fk_meta_package_permissions_meta_packages_meta_package_id foreign key (meta_package_id) references main.meta_packages(id) on delete restrict
);