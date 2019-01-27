import 'whatwg-fetch'

import { Query, Response as DataResponse } from '.'


export default class ApiClient {
  private baseUrl: string
  private baseOpts: RequestInit

  constructor(baseUrl: URL, baseOpts: RequestInit) {
    this.baseUrl = baseUrl.href
    this.baseOpts = Object.assign({}, baseOpts, {
      headers: {
        'Accept': 'application/json',
      },
      credentials: 'include',
      mode: 'cors',
    })
  }

  public get<T>(url: string, query?: Query): Promise<DataResponse<T>> {
    return this.send<T>('get', url, query)
  }

  public post<T = void>(url: string, query?: Query, body?: any): Promise<DataResponse<T>> {
    return this.send<T>('post', url, query, body)
  }

  public put<T = void>(url: string, query?: Query, body?: any): Promise<DataResponse<T>> {
    return this.send<T>('put', url, query, body)
  }

  public delete<T = void>(url: string, query?: Query): Promise<DataResponse<T>> {
    return this.send<T>('delete', url, query)
  }

  private send<T>(method: string, url: string, query?: Query, body?: any): Promise<DataResponse<T>> {
    return fetch(this.baseUrl + this.withQuery(url, query), this.prepareOptions(method, body))
      .then(
        response => this.readResponse(response).then(raw => this.parseResponse<T>(raw)),
        reason => this.parseFailure<T>(reason)
      )
  }

  private withQuery(url: string, query?: Query) {
    if (!query || !Object.keys(query).length)
      return url

    const params = Object.keys(query).map(param => `${param}=${encodeURIComponent(query[param].toString())}`).join('&')

    return `${url}?${params}`
  }

  private prepareOptions(method: string, body: any): RequestInit {
    const preparedBody = body ? this.prepareBody(body) : null

    const headers: { [key: string]: string } = {}

    if (typeof preparedBody === 'string')
      headers['Content-Type'] = 'application/json'

    return Object.assign({}, this.baseOpts, { method, headers, body: preparedBody })
  }

  private prepareBody(body: any): FormData | string {
    // if no files - send as json
    if (!Object.values(body).some(f => f instanceof Blob))
      return JSON.stringify(body)

    const data = new FormData()
    this.prepareFormData(data, body)

    return data
  }

  private prepareFormData(data: FormData, object: any, prefix?: string): void {
    for (const name in object)
      if (object.hasOwnProperty(name)) {
        const value = object[name]
        const prefixedName = prefix ? `${prefix}[${name}]` : name
        if (typeof value === 'string' || typeof value === 'number')
          data.append(prefixedName, value.toString())
        else if (typeof value === 'boolean')
          data.append(prefixedName, value ? '1' : '0')
        else if (value === null)
          data.append(prefixedName, 'null')
        else if (value instanceof File)
          data.append(prefixedName, value, value.name)
        else if (value instanceof Blob)
          data.append(prefixedName, value, name)
        else
          this.prepareFormData(data, value, prefixedName)
      }
  }

  private readResponse(response: Response): Promise<RawResponse> {
    return response.text().then(raw => ({
      isOk: response.ok,
      status: response.status,
      statusText: response.statusText,
      body: JSON.parse(raw),
    }))
  }

  private parseResponse<T>(raw: RawResponse): DataResponse<T> {
    if (raw.isOk && !raw.body.errors)
      return new DataResponse<T>(raw.body as T, null)

    return new DataResponse<T>(null as any as T, raw.body.toString())
  }

  private parseFailure<T>(reason: any): DataResponse<T> {
    return new DataResponse<T>(null as any as T, reason.toString())
  }
}

interface RawResponse {
  isOk: boolean
  status: number
  statusText: string
  body: any
}