import Icon from 'antd/lib/icon'
import React from 'react'

import MetaPackage from '../../models/view/MetaPackage'

import styles from './index.module.scss'


type Props = {
  metaPackage: MetaPackage
}

export default class PackageOwner extends React.PureComponent<Props> {
  render() {
    const { metaPackage } = this.props

    return (
      <div>
        <div className={styles.header}>Owner</div>
        <div className={styles.item}>
          <Icon className={styles.icon} type="user" />
          <span className={styles.label}>{metaPackage.owner}</span>
        </div>
      </div>
    )
  }
}