import Button from 'antd/lib/button'
import message from 'antd/lib/message'
import Switch from 'antd/lib/switch'
import { cloneDeep, isEqual } from 'lodash'
import { computed, observable } from 'mobx'
import { observer } from 'mobx-react'
import React from 'react'

import * as metaPackagesApi from '../../api/metaPackages'
import { MetaPackage } from '../../models/view/MetaPackage'
import { MetaPackagePermission } from '../../models/view/MetaPackagePermission'
import { Permission } from '../../models/view/Permission'
import { PermissionCategory } from '../../models/view/PermissionCategory'

import styles from './index.module.scss'


type Props = {
  metaPackage: MetaPackage
}

@observer
export class PackagePermissions extends React.Component<Props> {
  private static readonly categories = Object.keys(PermissionCategory)
    .filter(key => isNaN(parseInt(key, 10)))
    .map(key => key as keyof typeof PermissionCategory)

  private static readonly permissions = Object.keys(Permission)
    .filter(key => isNaN(parseInt(key, 10)))
    .map(key => key as keyof typeof Permission)
    .filter(key => Permission[key] > 0)

  @observable private readonly permissions: MetaPackagePermission[]

  @computed get hasChanges() {
    return !isEqual(this.permissions, this.props.metaPackage.permissions)
  }

  constructor(props: Props) {
    super(props)

    this.permissions = cloneDeep(this.props.metaPackage.permissions)
  }

  public render() {
    const getPermissionChecked = this.getPermissionChecked(this.permissions)
    const setPermissionChecked = this.setPermissionChecked(this.permissions)

    return (
      <div className={styles.block}>
        <div className={styles.header}>Permissions</div>
        {PackagePermissions.categories.map(category => (
          <div key={category} className={styles.category}>
            <div className={styles.label}>{category}</div>
            {PackagePermissions.permissions.map(permission => (
              <div key={permission} className={styles.permission}>
                <div className={styles.label}>{permission}</div>
                <Switch
                  className={styles.switch}
                  checked={getPermissionChecked(PermissionCategory[category], Permission[permission])}
                  onChange={setPermissionChecked(PermissionCategory[category], Permission[permission])}
                />
              </div>
            ))}
          </div>
        ))}
        <Button className={styles.submit} size="small" disabled={!this.hasChanges} onClick={this.updatePermissions}>
          Update permissions
        </Button>
      </div >
    )
  }

  private getPermissionChecked(permissions: MetaPackagePermission[]) {
    return (category: PermissionCategory, permission: Permission): boolean => {
      const packagePermission = permissions.find(p => p.category === category)!

      return Boolean(packagePermission.permission & permission)
    }
  }

  private setPermissionChecked(permissions: MetaPackagePermission[]) {
    return (category: PermissionCategory, permission: Permission) => (value: boolean): void => {
      const packagePermission = permissions.find(p => p.category === category)!

      packagePermission.permission = value
        ? packagePermission.permission | permission
        : packagePermission.permission & ~permission
    }
  }

  private readonly updatePermissions = () => {
    const { metaPackage } = this.props

    metaPackagesApi.setPermissions(metaPackage.type, metaPackage.name, this.permissions)
      .then(() => {
        message.success('Permissions updated')
        metaPackage.permissions = cloneDeep(this.permissions)
      })
      .catch(error => message.error(`Permissions update failed with: ${error}`))
  }
}
