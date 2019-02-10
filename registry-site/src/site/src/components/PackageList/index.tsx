import List from 'antd/lib/list'
import React from 'react'

import MetaPackage from '../../models/view/MetaPackage'
import PackageItem from '../PackageItem'

type Props = {
  packages: MetaPackage[]
}

export default class PackageList extends React.Component<Props> {
  render() {
    const { packages } = this.props

    return (
      <List
        header={this.renderHeader(packages)}
        itemLayout="vertical"
        dataSource={packages}
        renderItem={this.renderPackage} />
    )
  }

  private renderHeader = (packages: MetaPackage[]) => {
    return (
      <h3>Total {packages.length} packages:</h3>
    )
  }

  private renderPackage = (pkg: MetaPackage, index: number) => {
    return (
      <PackageItem key={index} pkg={pkg} />
    )
  }
}