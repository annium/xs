import Button from 'antd/lib/button'
import message from 'antd/lib/message'
import Switch from 'antd/lib/switch'
import { cloneDeep, isEqual } from 'lodash'
import { observer, useComputed, useObservable } from 'mobx-react-lite'
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

const Categories = Object.keys(PermissionCategory)
  .filter(key => isNaN(parseInt(key, 10)))
  .map(key => key as keyof typeof PermissionCategory)

const Permissions = Object.keys(Permission)
  .filter(key => isNaN(parseInt(key, 10)))
  .map(key => key as keyof typeof Permission)
  .filter(key => Permission[key] > 0)


export const PackagePermissions = observer(({ metaPackage }: Props) => {
  const permissions: MetaPackagePermission[] = useObservable(cloneDeep(metaPackage.permissions))
  const hasChanges = useComputed(() => !isEqual(permissions, metaPackage.permissions))
  const getPermissionChecked = createGetPermissionChecked(permissions)
  const setPermissionChecked = createSetPermissionChecked(permissions)

  return (
    <div className={styles.block}>
      <div className={styles.header}>Permissions</div>
      {Categories.map(category => (
        <div key={category} className={styles.category}>
          <div className={styles.label}>{category}</div>
          {Permissions.map(permission => (
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
      <Button
        className={styles.submit}
        size="small"
        disabled={!hasChanges}
        onClick={updatePermissions(metaPackage, permissions)}
      >
        Update permissions
      </Button>
    </div >
  )
})

const createGetPermissionChecked = (permissions: MetaPackagePermission[]) =>
  (category: PermissionCategory, permission: Permission): boolean => {
    const packagePermission = permissions.find(p => p.category === category)!

    return Boolean(packagePermission.permission & permission)
  }

const createSetPermissionChecked = (permissions: MetaPackagePermission[]) =>
  (category: PermissionCategory, permission: Permission) => (value: boolean): void => {
    const packagePermission = permissions.find(p => p.category === category)!

    packagePermission.permission = value
      ? packagePermission.permission | permission
      : packagePermission.permission & ~permission
  }

const updatePermissions = (metaPackage: MetaPackage, permissions: MetaPackagePermission[]) => () => metaPackagesApi
  .setPermissions(metaPackage.type, metaPackage.name, permissions)
  .then(() => {
    message.success('Permissions updated')
    metaPackage.permissions = cloneDeep(permissions)
  })
  .catch(error => message.error(`Permissions update failed with: ${error}`))

