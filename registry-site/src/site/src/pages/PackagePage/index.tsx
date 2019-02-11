import Col from 'antd/lib/col'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import { observable } from 'mobx'
import { observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router'

import metaPackages from '../../api/metaPackages'
import MetaPackage from '../../models/view/MetaPackage'
import { getCenteredLayout } from '../../utils/layout'
import { parseNameVersion } from '../../utils/nameVersion'

import styles from './index.module.scss'
import Package from './Package'

type Props = RouteComponentProps<{ type: string, nameVersion: string }>

@observer
export default class PackagePage extends React.Component<Props> {
  @observable private metaPackage: MetaPackage | null = null

  async componentDidMount() {
    const { type, nameVersion } = this.props.match.params
    const { name } = parseNameVersion(nameVersion)
    const packageResult = await metaPackages.get(type, name)

    if (packageResult.isSuccess)
      this.metaPackage = packageResult.data
    else
      message.error(`Package load failed with: ${packageResult.error}`)
  }

  render() {
    const { metaPackage } = this

    if (!metaPackage) return null

    const { version } = parseNameVersion(this.props.match.params.nameVersion)

    return (
      <div className={styles.page}>
        <Row>
          <Col {...getCenteredLayout(22, 22, 20, 18, 14)}>
            <Package metaPackage={metaPackage} version={version} />
          </Col>
        </Row>
      </div>
    )
  }
}