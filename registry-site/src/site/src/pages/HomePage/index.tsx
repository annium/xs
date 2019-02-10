import Col from 'antd/lib/col'
import Input from 'antd/lib/input'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import { observable } from 'mobx'
import { observer } from 'mobx-react'
import * as React from 'react'

import metaPackages from '../../api/metaPackages'
import PackageList from '../../components/PackageList'
import MetaPackage from '../../models/view/MetaPackage'
import { getCenteredLayout } from '../../utils/layout'

import styles from './index.module.scss'


const log = console.log.bind(console, 'HomePage')
@observer
export default class HomePage extends React.Component {
  @observable private query: string = ''
  @observable private packages: MetaPackage[] = []

  async componentDidMount() {
    const packagesResult = await metaPackages.search('', '', '', 1)

    if (packagesResult.isSuccess)
      this.packages = packagesResult.data
    else
      message.error(`Packages load failed with: ${packagesResult.error}`)
  }

  render() {
    log('render')
    const query = this.query.toLowerCase()
    const packages = query ? this.packages.filter(p => p.name.toLowerCase().includes(query)) : this.packages

    return (
      <div className={styles.page}>
        <Row>
          <Col {...getCenteredLayout(22, 22, 20, 18, 14)}>
            <h1>My packages</h1>
            <Input.Search placeholder="search packages" enterButton onSearch={this.setQuery} />
            <PackageList packages={packages} />
          </Col>
        </Row>
      </div>
    )
  }

  private setQuery = (value: string) => this.query = value
}