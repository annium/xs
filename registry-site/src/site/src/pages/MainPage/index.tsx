import Col, { ColSize } from 'antd/lib/col'
import Input from 'antd/lib/input'
import Row from 'antd/lib/row'
import { observable } from 'mobx'
import { observer } from 'mobx-react'
import * as React from 'react'

import styles from './index.module.scss'


const log = console.log.bind(console, 'MainPage')
@observer
export default class MainPage extends React.Component {
  @observable private query: string = ''

  render() {
    log('render')
    const { query } = this

    return (
      <div className={styles.page}>
        <Row>
          <Col {...this.getLayout()}>
            <h1>Main page</h1>
            <Input.Search placeholder="search packages" enterButton onSearch={this.setQuery} />
            <h2>{query}</h2>
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