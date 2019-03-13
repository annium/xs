import List from 'antd/lib/list'
import React from 'react'

import { MetaPackage } from '../../models/view/MetaPackage'
import { PackageItem } from '../PackageItem'


import styles from './index.module.scss'

type Props = {
  packages: MetaPackage[]
}

export class PackageList extends React.Component<Props> {
  public render() {
    const { packages } = this.props

    return (
      <>
        {this.renderHeader(packages)}
        <div className={styles.container}>
          <List
            itemLayout="vertical"
            dataSource={packages}
            renderItem={this.renderPackage}
          />
        </div>
      </>
    )
  }

  private renderHeader(packages: MetaPackage[]) {
    return (
      <h2 className={styles.header}>Total {packages.length} packages:</h2>
    )
  }

  private renderPackage(pkg: MetaPackage, index: number) {
    return (
      <PackageItem key={index} pkg={pkg} />
    )
  }
}
