import Col, { ColSize } from 'antd/lib/col'
import Input from 'antd/lib/input'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import { observable } from 'mobx'
import { observer } from 'mobx-react'
import * as React from 'react'

import metaPackages from '../../api/metaPackages'
import PackageList from '../../components/PackageList'
import MetaPackage from '../../models/view/MetaPackage'

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
          <Col {...this.getLayout()}>
            <h1>My packages</h1>
            <Input.Search placeholder="search packages" enterButton onSearch={this.setQuery} />
            <PackageList packages={packages} />
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

  private setQuery = (value: string) => this.query = value
}