import Avatar from 'antd/lib/avatar'
import React from 'react'

import MetaPackage from '../../models/view/MetaPackage'
import Package from '../../models/view/Package'

import styles from './index.module.scss'


type Props = {
  metaPackage: MetaPackage
  pkg: Package
}

export default class PackageTitle extends React.PureComponent<Props>{
  render() {
    const { metaPackage, pkg } = this.props

    return (
      <>
        <div className={styles.header}>
          <Avatar src={`/icons/${metaPackage.type}.svg`} size="large" />
          <div className={styles.nameVersion}>
            <span className={styles.name}>{pkg.name}</span>
            <span className={styles.version}>{pkg.version}</span>
          </div>
        </div>
        <div className={styles.description}>{pkg.description}</div>
      </>
    )
  }
}