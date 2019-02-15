import Input from 'antd/lib/input'
import React from 'react'

import { ProjectType } from '../../models/view/ProjectType'
import ProjectTypeSelect from '../ProjectTypeSelect'

import styles from './index.module.scss'

type Props = {
  type: ProjectType
  onTypeChange: (type: ProjectType) => void
  query: string
  onQueryChange: (query: string) => void
  onSubmit: () => void
}

export default class PackageFilter extends React.PureComponent<Props> {
  render() {
    const { type, onTypeChange, query, onQueryChange, onSubmit } = this.props

    const handleQueryChange = (e: React.ChangeEvent<HTMLInputElement>) => onQueryChange(e.target.value)

    return (
      <div className={styles.filter}>
        <ProjectTypeSelect type={type} onSelect={onTypeChange} />
        <Input.Search
          placeholder="search packages"
          enterButton
          value={query}
          onChange={handleQueryChange}
          onSearch={onSubmit} />
      </div>
    )
  }
}