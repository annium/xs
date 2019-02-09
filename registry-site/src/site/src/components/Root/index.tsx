import * as React from 'react'

import styles from './index.module.scss'
import Menu from './Menu'


export default class Root extends React.Component {
  render() {
    return (
      <div className={styles.root}>
        <div className={styles.content}>
          {this.props.children}
        </div>
        <Menu />
      </div>
    )
  }
}