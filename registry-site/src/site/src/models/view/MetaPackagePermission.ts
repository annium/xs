import { Permission } from './Permission'
import { PermissionCategory } from './PermissionCategory'

export default interface MetaPackagePermission {
  category: PermissionCategory
  permission: Permission
}