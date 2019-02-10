import Avatar from 'antd/lib/avatar'
import Icon from 'antd/lib/icon'
import List from 'antd/lib/list'
import cx from 'classnames'
import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { NavLink } from 'react-router-dom'

import MetaPackage from '../../models/view/MetaPackage'
import { Permission } from '../../models/view/Permission'
import UserMetaPackageAccess from '../../models/view/UserMetaPackageAccess'
import { Store } from '../../store'

import styles from './index.module.scss'


type Props = Partial<Pick<Store, 'user'>> & {
  pkg: MetaPackage
}

@inject((stores: Store) => ({ user: stores.user }))
@observer
export default class PackageItem extends React.Component<Props> {
  private static readonly permissionKeys = Object.keys(Permission)
    .filter(key => typeof key === 'string')
    .map(key => key as keyof typeof Permission)

  render() {
    const { pkg, user } = this.props

    const access = new UserMetaPackageAccess(user!.data!.id, pkg.ownerId, pkg.permissions)

    return (
      <List.Item>
        <List.Item.Meta
          avatar={<Avatar src={`/icons/${pkg.type}.svg`} />}
          title={this.renderPackageTitle(pkg, access)}
          description={this.renderPackageDetails(pkg)} />
        {pkg.description}
      </List.Item>
    )
  }

  private renderPackageTitle = (pkg: MetaPackage, access: UserMetaPackageAccess) => {
    return (
      <div className={styles.title}>
        <NavLink className={styles.name} to={`/packages/${pkg.type}/${pkg.name}`}>{pkg.name}</NavLink>
        {this.renderPackageAccess(pkg.owner, access)}
      </div>
    )
  }

  private renderPackageAccess = (owner: string, access: UserMetaPackageAccess) => {
    const ownerCls = cx({
      [styles.isOwner]: access.isOwner,
      [styles.isWorld]: access.isWorld
    })

    return (
      <div className={styles.access}>
        <span className={styles.owner}>
          by: <span className={ownerCls}>{owner}</span>
        </span>
        <span className={styles.permissions}>{this.getPermissionList(access.permission)}</span>
      </div>
    )
  }

  private renderPackageDetails = (pkg: MetaPackage) => {
    return (
      <div className={styles.details}>
        <span><Icon type="download" /> {pkg.downloads.toLocaleString()} total downloads</span>
        <span><Icon type="clock-circle" /> last updated {pkg.published.fromNow()}</span>
        <span><Icon type="flag" /> latest version: {pkg.version}</span>
      </div>
    )
  }

  private getPermissionList(permission: Permission) {
    return PackageItem.permissionKeys
      .filter(name => Permission[name] && (permission & (Permission[name] as Permission)) === Permission[name])
      .join(', ')
  }
}