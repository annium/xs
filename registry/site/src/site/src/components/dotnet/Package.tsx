import { Col, Row } from 'antd/lib/grid'
import message from 'antd/lib/message'
import confirm from 'antd/lib/modal/confirm'
import _ from 'lodash'
import { computed, observable } from 'mobx'
import { observer } from 'mobx-react'
import React from 'react'

import { api as serverApi } from '../../api/server/dotnet'
import { Package as PackageModel } from '../../models/view/dotnet/Package'
import { MetaPackage } from '../../models/view/MetaPackage'
import { UserMetaPackageAccess } from '../../models/view/UserMetaPackageAccess'
import { gutter } from '../../utils/layout'
import { PackageInfo } from '../PackageInfo'
import { PackageOwner } from '../PackageOwner'
import { PackagePermissions } from '../PackagePermissions'
import { PackageStats } from '../PackageStats'
import { PackageTitle } from '../PackageTitle'
import { PackageVersions } from '../PackageVersions'

import { Dependencies } from './Dependencies'


type Props = {
  access: UserMetaPackageAccess
  metaPackage: MetaPackage
  version?: string
}

@observer
export class Package extends React.Component<Props> {
  @observable private packages: PackageModel[] = []

  @computed public get pkg() {
    const { version } = this.props

    return version ?
      this.packages.filter(p => p.version === version)[0] :
      _.sortBy(this.packages, (pkg: PackageModel) => pkg.version)[this.packages.length - 1]
  }

  public async componentDidMount() {
    await this.loadPackages(this.props.metaPackage.name)
  }

  public async componentDidUpdate(prevProps: Props) {
    if (this.props.metaPackage !== prevProps.metaPackage)
      await this.loadPackages(this.props.metaPackage.name)
  }

  public render() {
    const { pkg, packages } = this
    const { access, metaPackage } = this.props

    if (!pkg) return null

    return (
      <Row gutter={gutter}>
        <Col span={16}>
          <PackageTitle access={access} metaPackage={metaPackage} pkg={pkg} onDelete={this.handleDelete} />
          <Dependencies dependencies={pkg.dependencies} />
          <PackageVersions type={metaPackage.type} pkg={pkg} packages={packages} />
        </Col>
        <Col span={8}>
          <PackageInfo pkg={pkg} />
          {access.isOwner ? <PackagePermissions metaPackage={metaPackage} /> : undefined}
          <PackageStats pkg={pkg} packages={packages} />
          <PackageOwner metaPackage={metaPackage} />
        </Col>
      </Row>
    )
  }

  private async loadPackages(name: string) {
    const packagesResult = await serverApi.get(name)

    if (packagesResult.isSuccess)
      this.packages = packagesResult.data
    else
      message.error(`Package load failed with: ${packagesResult.error}`)
  }

  private readonly handleDelete = () => {
    const { name, version } = this.pkg
    confirm({
      title: 'Confirm delete',
      content: <span>Confirm, if you really want to delete package <b>{name} {version}</b></span>,
      onOk: () => serverApi.delete(name, version)
        .then(() => message.success('Package successfully deleted'))
        .catch(error => message.error(`Package deletion failed with: ${error}`)),
      maskClosable: true,
    })
  }
}
