import Icon from 'antd/lib/icon'
import _ from 'lodash'
import React from 'react'

import Package from '../../models/view/Package'

import styles from './index.module.scss'


type Props = {
  pkg: Package
  packages: Package[]
}

export default class PackageStats extends React.PureComponent<Props> {
  render() {
    const { pkg, packages } = this.props

    return (
      <div>
        <div className={styles.header}>Stats</div>
        <div className={styles.item}>
          <Icon className={styles.icon} type="download" />
          <span className={styles.label}>
            {_.sum(packages.map(p => p.downloads)).toLocaleString()} total downloads
          </span>
        </div>
        <div className={styles.item}>
          <Icon className={styles.icon} type="gift" />
          <span className={styles.label}>
            {pkg.downloads.toLocaleString()} downloads of current version</span>
        </div>
      </div>
    )
  }
}