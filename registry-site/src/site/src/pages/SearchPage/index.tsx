import Col, { ColSize } from 'antd/lib/col'
import Input from 'antd/lib/input'
import List from 'antd/lib/list'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import { observable } from 'mobx'
import { observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router'

import metaPackages from '../../api/metaPackages'
import PackageItem from '../../components/PackageItem'
import PackageList from '../../components/PackageList'
import ProjectTypeSelect from '../../components/ProjectTypeSelect'
import MetaPackage from '../../models/view/MetaPackage'
import { ProjectType } from '../../models/view/ProjectType'
import { updateLocation } from '../../utils/history'

import styles from './index.module.scss'


type Props = RouteComponentProps

@observer
export default class SearchPage extends React.Component<Props> {
  @observable private type: ProjectType
  @observable private query: string
  @observable private packages: MetaPackage[] = []

  constructor(props: Props) {
    super(props)

    const params = new URLSearchParams(props.location.search)
    this.type = Object.values(ProjectType).includes(params.get('type'))
      ? params.get('type') as ProjectType
      : ProjectType.Any
    this.query = params.get('query') || ''
  }

  componentDidMount() {
    this.search()
  }

  render() {
    const { type, query, packages } = this

    return (
      <div className={styles.page}>
        <Row>
          <Col {...this.getLayout()}>
            <h1>My packages</h1>
            <div className={styles.filter}>
              <ProjectTypeSelect type={type} onSelect={this.setType} />
              <Input.Search
                placeholder="search packages"
                enterButton
                value={this.query}
                onChange={this.setQuery}
                onSearch={this.search} />
            </div>
            <PackageList packages={packages} />
          </Col>
        </Row>
      </div>
    )
  }

  private search = async () => {
    const { type, query } = this
    updateLocation(this.props.history, { type, query })
    const packagesResult = await metaPackages.search('', type, query, 1)

    if (packagesResult.isSuccess)
      this.packages = packagesResult.data
    else
      message.error(`Packages load failed with: ${packagesResult.error}`)
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

  private setType = (type: ProjectType) => this.type = type

  private setQuery = (e: React.ChangeEvent<HTMLInputElement>) => this.query = e.target.value
}