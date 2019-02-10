import Col, { ColSize } from 'antd/lib/col'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import { observable } from 'mobx'
import { observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router'

import metaPackages from '../../api/metaPackages'
import MetaPackage from '../../models/view/MetaPackage'

import styles from './index.module.scss'

type Props = RouteComponentProps<{ type: string, name: string }>

const log = console.log.bind(console, 'PackagePage')
@observer
export default class PackagePage extends React.Component<Props> {
  @observable private metaPackage: MetaPackage | null = null

  async componentDidMount() {
    const { type, name } = this.props.match.params
    const packageResult = await metaPackages.get(type, name)

    if (packageResult.isSuccess)
      this.metaPackage = packageResult.data
    else
      message.error(`Package load failed with: ${packageResult.error}`)
  }

  render() {
    log('render')
    const { metaPackage } = this

    if (!metaPackage) return null

    return (
      <div className={styles.page}>
        <Row>
          <Col {...this.getLayout()}>
            <h1>{metaPackage.type} {metaPackage.name}</h1>
          </Col>
        </Row>
      </div>
    )
  }

  private getLayout(): { [key: string]: ColSize } {
    return {
      xs: { offset: 1, span: 22 },
      sm: { offset: 1, span: 22 },
      md: { offset: 2, span: 20 },
      lg: { offset: 3, span: 18 },
      xl: { offset: 5, span: 14 },
    }
  }
}