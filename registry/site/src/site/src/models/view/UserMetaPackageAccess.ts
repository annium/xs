import MetaPackagePermission from './MetaPackagePermission'
import { Permission } from './Permission'
import { PermissionCategory } from './PermissionCategory'

export default class UserMetaPackageAccess {
  readonly isOwner: boolean
  readonly isWorld: boolean
  readonly permission: Permission

  constructor(
    userId: string,
    ownerId: string,
    permissions: MetaPackagePermission[]
  ) {
    const category = ownerId === userId ? PermissionCategory.Owner : PermissionCategory.World

    this.isOwner = category === PermissionCategory.Owner
    this.isWorld = category === PermissionCategory.World

    permissions.find(p => p.category === category)

    this.permission = permissions.find(p => p.category === category)!.permission
  }

  has(permission: Permission) {
    return (this.permission & permission) === permission
  }
}