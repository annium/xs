import Avatar from 'antd/lib/avatar'
import Col, { ColSize } from 'antd/lib/col'
import Icon from 'antd/lib/icon'
import Input from 'antd/lib/input'
import List from 'antd/lib/list'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import { observable } from 'mobx'
import { observer } from 'mobx-react'
import * as React from 'react'
import { NavLink } from 'react-router-dom'

import packagesApi from '../../api/packages'
import MetaPackage from '../../models/view/MetaPackage'

import styles from './index.module.scss'


const log = console.log.bind(console, 'MainPage')
@observer
export default class MainPage extends React.Component {
  @observable private query: string = ''
  @observable private packages: MetaPackage[] = []

  async componentDidMount() {
    const packagesResult = await packagesApi.my()

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
            <h1>Main page</h1>
            <Input.Search placeholder="search packages" enterButton onSearch={this.setQuery} />
            {this.renderPackages(packages)}
          </Col>
        </Row>
      </div>
    )
  }

  private renderPackages = (packages: MetaPackage[]) => {
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
      <List.Item key={index}>
        <List.Item.Meta
          avatar={<Avatar src={`/icons/${pkg.type}.svg`} />}
          title={this.renderPackageTitle(pkg)}
          description={this.renderPackageDetails(pkg)} />
        {pkg.description}
      </List.Item>
    )
  }

  private renderPackageTitle = (pkg: MetaPackage) => {
    return (
      <div className={styles.pkgMetaTitle}>
        <NavLink className={styles.pkgMetaName} to={`/packages/${pkg.type}/${pkg.name}`}>{pkg.name}</NavLink>
        <span className={styles.pkgMetaOwner}>by: {pkg.owner}</span>
      </div>
    )
  }

  private renderPackageDetails = (pkg: MetaPackage) => {
    return (
      <div className={styles.pkgMetaDetails}>
        <span><Icon type="download" /> {pkg.downloads.toLocaleString()} total downloads</span>
        <span><Icon type="clock-circle" /> last updated {pkg.published.fromNow()}</span>
        <span><Icon type="flag" /> latest version: {pkg.version}</span>
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