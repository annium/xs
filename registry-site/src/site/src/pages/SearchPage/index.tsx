import Col from 'antd/lib/col'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import { observable } from 'mobx'
import { observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router'

import metaPackages from '../../api/metaPackages'
import PackageFilter from '../../components/PackageFilter'
import PackageList from '../../components/PackageList'
import MetaPackage from '../../models/view/MetaPackage'
import { ProjectType } from '../../models/view/ProjectType'
import { updateLocation } from '../../utils/history'
import { getCenteredLayout } from '../../utils/layout'

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
          <Col {...getCenteredLayout(22, 22, 20, 18, 14)}>
            <h1>My packages</h1>
            <PackageFilter
              type={type}
              onTypeChange={this.setType}
              query={query}
              onQueryChange={this.setQuery}
              onSubmit={this.search} />
            <PackageList packages={packages} />
          </Col>
        </Row>
      </div >
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

  private setType = (type: ProjectType) => this.type = type

  private setQuery = (query: string) => this.query = query
}