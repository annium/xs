import { Permission } from './Permission'
import { PermissionCategory } from './PermissionCategory'

export type MetaPackagePermission = {
  category: PermissionCategory
  permission: Permission
}
