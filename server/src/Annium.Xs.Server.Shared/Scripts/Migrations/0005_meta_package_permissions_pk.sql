-- 0003 declared the primary key as (meta_package_id, category, permission), which includes the mutable
-- value column. That does not enforce the one-row-per-category invariant the code assumes:
-- UserMetaPackageAccess resolves a user's effective permission with FirstOrDefault(p => p.Category == ...),
-- so two rows sharing (meta_package_id, category) with different permission values would make the
-- authorization outcome depend on row order. Narrow the key to (meta_package_id, category).

-- collapse any pre-existing duplicates first so the narrowed key can be created. No application path
-- produces duplicates (CreateAsync inserts exactly one row per category and UpdatePermissionsAsync only
-- UPDATEs), but a direct insert may have. `permission` is a [Flags] enum, so the union of a duplicate
-- group's grants is a bitwise OR, NOT max(): between Read|Publish (3) and Unpublish (4), max() would
-- pick 4 and silently drop two capabilities that were actually in effect.
create temporary table meta_package_permissions_merged as
select meta_package_id, category, bit_or(permission) as permission
from main.meta_package_permissions
group by meta_package_id, category
having count(*) > 1;

delete from main.meta_package_permissions p using meta_package_permissions_merged m
where p.meta_package_id = m.meta_package_id and p.category = m.category;

insert into main.meta_package_permissions (meta_package_id, category, permission)
select meta_package_id, category, permission
from meta_package_permissions_merged;

drop table meta_package_permissions_merged;

alter table main.meta_package_permissions drop constraint pk_meta_package_permissions;

alter table main.meta_package_permissions
add constraint pk_meta_package_permissions primary key (meta_package_id, category);
