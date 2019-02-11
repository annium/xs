import { Col, Row } from 'antd/lib/grid'
import message from 'antd/lib/message'
import confirm from 'antd/lib/modal/confirm'
import _ from 'lodash'
import { computed, observable } from 'mobx'
import { observer } from 'mobx-react'
import React from 'react'

import serverApi from '../../../api/server/node'
import MetaPackage from '../../../models/view/MetaPackage'
import PackageModel from '../../../models/view/node/Package'
import { gutter } from '../../../utils/layout'
import PackageTitle from '../../PackageTitle'
import PackageVersions from '../../PackageVersions'


type Props = {
  metaPackage: MetaPackage
  version?: string
}

@observer
export default class Package extends React.Component<Props>{
  @observable private packages: PackageModel[] = []

  @computed public get pkg() {
    const { version } = this.props

    return version ?
      this.packages.filter(p => p.version === version)[0] :
      _.sortBy(this.packages, (pkg: PackageModel) => pkg.version)[this.packages.length - 1]
  }

  async componentDidMount() {
    const { metaPackage: { name } } = this.props

    const packagesResult = await serverApi.get(name)

    if (packagesResult.isSuccess)
      this.packages = packagesResult.data
    else
      message.error(`Package load failed with: ${packagesResult.error}`)
  }

  render() {
    const { pkg, packages } = this
    const { metaPackage } = this.props

    if (!pkg) return null

    return (
      <Row gutter={gutter}>
        <Col span={16}>
          <PackageTitle metaPackage={metaPackage} pkg={pkg} onDelete={this.handleDelete} />
          <PackageVersions type={metaPackage.type} pkg={pkg} packages={packages} />
        </Col>
        <Col span={8}>
          info, etc, here
        </Col>
      </Row>
    )
  }

  private handleDelete = () => {
    const { name, version } = this.pkg
    confirm({
      title: 'Confirm delete',
      content: <span>Confirm, if you really want to delete package <b>{name} {version}</b></span>,
      onOk: () => serverApi.delete(name, version).then(
        () => message.success('Package successfully deleted'),
        error => message.error(`Package deletion failed with: ${error}`)
      ),
      maskClosable: true,
    })
  }
}