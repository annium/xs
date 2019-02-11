import Icon from 'antd/lib/icon'
import React from 'react'

import Package from '../../models/view/Package'

import styles from './index.module.scss'


type Props = {
  pkg: Package
}

export default class PackageInfo extends React.PureComponent<Props> {
  render() {
    const { pkg } = this.props

    return (
      <div>
        <div className={styles.header}>Info</div>
        <div className={styles.item}>
          <Icon className={styles.icon} type="clock-circle" />
          <span className={styles.label}>published {pkg.published.fromNow()}</span>
        </div>
      </div >
    )
  }
}