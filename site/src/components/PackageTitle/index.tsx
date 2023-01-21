import Avatar from 'antd/lib/avatar'
import Button from 'antd/lib/button'
import React from 'react'

import { MetaPackage } from '../../models/view/MetaPackage'
import { Package } from '../../models/view/Package'
import { Permission } from '../../models/view/Permission'
import { UserMetaPackageAccess } from '../../models/view/UserMetaPackageAccess'

import styles from './index.module.scss'


type Props = {
  access: UserMetaPackageAccess
  metaPackage: MetaPackage
  pkg: Package
  onDelete(): void;
}

export const PackageTitle = ({ access, metaPackage, pkg, onDelete: handleDelete }: Props) => (
  <>
    <div className={styles.header}>
      <Avatar src={`/icons/${metaPackage.type}.svg`} size="large" />
      <div className={styles.nameVersion}>
        <span className={styles.name}>{pkg.name}</span>
        <span className={styles.version}>{pkg.version}</span>
      </div>
      <div className={styles.separator} />
      {access.has(Permission.Unpublish)
        ? <Button icon="delete" onClick={handleDelete}>Delete</Button>
        : undefined}
    </div>
    <div className={styles.description}>{pkg.description}</div>
  </>
)
