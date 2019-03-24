import { MetaPackagePermission } from './MetaPackagePermission'
import { Permission } from './Permission'
import { PermissionCategory } from './PermissionCategory'

export class UserMetaPackageAccess {
  public readonly isOwner: boolean
  public readonly isWorld: boolean
  public readonly permission: Permission

  constructor(
    userId: string,
    ownerId: string,
    permissions: MetaPackagePermission[],
  ) {
    const category = ownerId === userId ? PermissionCategory.Owner : PermissionCategory.World

    this.isOwner = category === PermissionCategory.Owner
    this.isWorld = category === PermissionCategory.World

    this.permission = permissions.find(p => p.category === category)!.permission
  }

  public has(permission: Permission) {
    return (this.permission & permission) === permission
  }
}
