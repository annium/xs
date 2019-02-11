import message from 'antd/lib/message'
import confirm from 'antd/lib/modal/confirm'
import { observable } from 'mobx'
import { observer } from 'mobx-react'
import React from 'react'

import serverApi from '../../../api/server/dotnet'
import PackageModel from '../../../models/view/dotnet/Package'
import MetaPackage from '../../../models/view/MetaPackage'
import PackageTitle from '../../PackageTitle'

import styles from './index.module.scss'

type Props = {
  metaPackage: MetaPackage
  version?: string
}

@observer
export default class Package extends React.Component<Props>{
  @observable private pkg: PackageModel | null = null

  async componentDidMount() {
    const { metaPackage: { name }, version } = this.props

    const packageResult = version
      ? await serverApi.get(name, version)
      : await serverApi.getLatest(name)

    if (packageResult.isSuccess)
      this.pkg = packageResult.data
    else
      message.error(`Package load failed with: ${packageResult.error}`)
  }

  render() {
    const { pkg } = this
    const { metaPackage } = this.props

    if (!pkg) return null

    return (
      <div className={styles.package}>
        <PackageTitle metaPackage={metaPackage} pkg={pkg} onDelete={this.handleDelete} />
      </div>
    )
  }

  private handleDelete = () => {
    const { name, version } = this.pkg!
    confirm({
      title: 'Confirm delete',
      content: `Confirm, if you really want to delete package ${name}:${version}`,
      onOk: () => serverApi.delete(name, version).then(
        () => message.success('Package successfully deleted'),
        error => message.error(`Package deletion failed with: ${error}`)
      ),
      maskClosable: true,
    })
  }
}