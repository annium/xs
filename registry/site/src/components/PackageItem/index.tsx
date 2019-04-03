import Avatar from 'antd/lib/avatar'
import Icon from 'antd/lib/icon'
import List from 'antd/lib/list'
import cx from 'classnames'
import React from 'react'
import { NavLink } from 'react-router-dom'

import { MetaPackage } from '../../models/view/MetaPackage'
import { Permission } from '../../models/view/Permission'
import { UserMetaPackageAccess } from '../../models/view/UserMetaPackageAccess'
import { connect, Store } from '../../store'
import * as route from '../../utils/route'

import styles from './index.module.scss'


type Props = Partial<Pick<Store, 'auth'>> & {
  pkg: MetaPackage
}

const permissionKeys = Object.keys(Permission)
  .filter((key: string | number) => typeof key === 'string')
  .map(key => key as keyof typeof Permission)

export const PackageItem = connect<Props, Pick<Store, 'auth'>>(
  ({ auth }) => ({ auth }),
  function PackageItem({ pkg, auth }: Props) {
    const access = new UserMetaPackageAccess(auth!.user.data!.id, pkg.ownerId, pkg.permissions)

    return (
      <List.Item>
        <List.Item.Meta
          avatar={<Avatar src={`/icons/${pkg.type}.svg`} />}
          title={renderPackageTitle(pkg, access)}
          description={renderPackageDetails(pkg)}
        />
        {pkg.description}
      </List.Item>
    )
  },
)

const renderPackageTitle = (pkg: MetaPackage, access: UserMetaPackageAccess) => (
  <div className={styles.title}>
    <NavLink className={styles.name} to={route.pkg(pkg.type, pkg.name)}>{pkg.name}</NavLink>
    {renderPackageAccess(pkg.owner, access)}
  </div>
)

const renderPackageAccess = (owner: string, access: UserMetaPackageAccess) => {
  const ownerCls = cx({
    [styles.isOwner]: access.isOwner,
    [styles.isWorld]: access.isWorld,
  })

  return (
    <div className={styles.access}>
      <span className={styles.owner}>
        by: <span className={ownerCls}>{owner}</span>
      </span>
      <span className={styles.permissions}>{getPermissionList(access.permission)}</span>
    </div>
  )
}

const renderPackageDetails = (pkg: MetaPackage) => (
  <div className={styles.details}>
    <span><Icon type="download" /> {pkg.downloads.toLocaleString()} total downloads</span>
    <span><Icon type="clock-circle" /> last updated {pkg.published.fromNow()}</span>
    <span><Icon type="flag" /> latest version: {pkg.version}</span>
  </div>
)

const getPermissionList = (permission: Permission) => permissionKeys
  .filter(name => Permission[name] && (permission & (Permission[name] as Permission)) === Permission[name])
  .join(', ')

