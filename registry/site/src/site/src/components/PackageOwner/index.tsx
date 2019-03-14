import Icon from 'antd/lib/icon'
import React from 'react'

import { MetaPackage } from '../../models/view/MetaPackage'

import styles from './index.module.scss'


type Props = {
  metaPackage: MetaPackage
}

export const PackageOwner = ({ metaPackage }: Props) => (
  <div>
    <div className={styles.header}>Owner</div>
    <div className={styles.item}>
      <Icon className={styles.icon} type="user" />
      <span className={styles.label}>{metaPackage.owner}</span>
    </div>
  </div>
)
