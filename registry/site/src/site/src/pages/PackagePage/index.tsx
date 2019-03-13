import Col from 'antd/lib/col'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import { observable } from 'mobx'
import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps, withRouter } from 'react-router'

import * as metaPackagesApi from '../../api/metaPackages'
import { MetaPackage } from '../../models/view/MetaPackage'
import { UserMetaPackageAccess } from '../../models/view/UserMetaPackageAccess'
import { Store } from '../../store'
import { getCenteredLayout } from '../../utils/layout'
import { parseNameVersion } from '../../utils/nameVersion'

import styles from './index.module.scss'
import { Package } from './Package'


type Props = Pick<Store, 'user'> & RouteComponentProps<{ type: string, nameVersion: string }>

class PackagePageInternal extends React.Component<Props> {
  @observable private metaPackage?: MetaPackage

  public async componentDidMount() {
    const { type, nameVersion } = this.props.match.params
    await this.loadMetaPackage(type, nameVersion)
  }

  public async componentDidUpdate(prevProps: Props) {
    const prevParams = prevProps.match.params
    const params = this.props.match.params

    if (params.type !== prevParams.type || params.nameVersion !== prevParams.nameVersion)
      await this.loadMetaPackage(params.type, params.nameVersion)
  }

  public render() {
    const { metaPackage } = this
    if (!metaPackage) return null

    const { match, user } = this.props

    const { version } = parseNameVersion(match.params.nameVersion)
    const access = new UserMetaPackageAccess(user.data!.id, metaPackage.ownerId, metaPackage.permissions)

    console.warn('RENDER PackagePage')

    return (
      <div className={styles.page}>
        <Row>
          <Col {...getCenteredLayout(22, 22, 20, 18, 14)}>
            <Package access={access} metaPackage={metaPackage} version={version} />
          </Col>
        </Row>
      </div>
    )
  }

  private async loadMetaPackage(type: string, nameVersion: string) {
    const { name } = parseNameVersion(nameVersion)
    const packageResult = await metaPackagesApi.get(type, name)

    if (packageResult.isSuccess)
      this.metaPackage = packageResult.data
    else
      message.error(`Package load failed with: ${packageResult.error}`)
  }
}

export const PackagePage = withRouter(inject((stores: Store) => ({ user: stores.user }))(observer(PackagePageInternal)))
