import { merge } from 'lodash'

import { Query, Response as DataResponse } from '.'


type RawHeaders = Record<string, string>
type Body = { [key: string]: unknown }
type Stringifiable = { toString(): string }

export class Client {
  private readonly baseUrl: string
  private readonly baseOptions: RequestInit

  constructor(baseUrl: URL, baseOptions: RequestInit) {
    this.baseUrl = baseUrl.href
    this.baseOptions = merge(
      {
        credentials: 'include',
        headers: {
          Accept: 'application/json',
        },
        mode: 'cors',
      },
      baseOptions,
    )
  }

  public get<T>(url: string, query?: Query, headers?: RawHeaders): Promise<DataResponse<T>> {
    return this.send<T>('get', url, query, undefined, headers)
  }

  public post<T = void>(url: string, query?: Query, body?: Body, headers?: RawHeaders): Promise<DataResponse<T>> {
    return this.send<T>('post', url, query, body, headers)
  }

  public put<T = void>(url: string, query?: Query, body?: Body, headers?: RawHeaders): Promise<DataResponse<T>> {
    return this.send<T>('put', url, query, body, headers)
  }

  public delete<T = void>(url: string, query?: Query, headers?: RawHeaders): Promise<DataResponse<T>> {
    return this.send<T>('delete', url, query, undefined, headers)
  }

  private send<T>(
    method: string,
    url: string,
    query?: Query,
    body?: Body,
    headers?: RawHeaders,
  ): Promise<DataResponse<T>> {
    return fetch(
      this.baseUrl + this.withQuery(url, query),
      this.prepareOptions(method, headers || {}, body),
    )
      .then(
        (response: Response) => this.readResponse(response).then(raw => this.parseResponse<T>(raw)),
        (reason: Stringifiable) => this.parseFailure<T>(reason),
      )
  }

  private withQuery(url: string, query?: Query) {
    if (!query || !Object.keys(query).length)
      return url

    const params = Object.keys(query).map(param => `${param}=${encodeURIComponent(query[param].toString())}`).join('&')

    return `${url}?${params}`
  }

  private prepareOptions(method: string, headers: RawHeaders, body?: Body): RequestInit {
    const preparedBody = body ? this.prepareBody(body) : undefined

    if (typeof preparedBody === 'string')
      headers['Content-Type'] = 'application/json'

    return merge(this.baseOptions, { method, headers, body: preparedBody })
  }

  private prepareBody(body: Body): FormData | string {
    // if no files - send as json
    if (!Object.values(body as {}).some(f => f instanceof Blob))
      return JSON.stringify(body)

    const data = new FormData()
    this.prepareFormData(data, body)

    return data
  }

  private prepareFormData(data: FormData, object: Body, prefix?: string): void {
    for (const name in object)
      if (object.hasOwnProperty(name)) {
        const value: unknown = object[name] as unknown
        const prefixedName = prefix ? `${prefix}[${name}]` : name
        if (typeof value === 'string' || typeof value === 'number')
          data.append(prefixedName, value.toString())
        else if (typeof value === 'boolean')
          data.append(prefixedName, value ? '1' : '0')
        else if (value === undefined)
          data.append(prefixedName, 'undefined')
        else if (value instanceof File)
          data.append(prefixedName, value, value.name)
        else if (value instanceof Blob)
          data.append(prefixedName, value, name)
        else
          this.prepareFormData(data, value as Body, prefixedName)
      }
  }

  private readResponse(response: Response): Promise<RawResponse> {
    return response.text().then(body => ({
      body: this.parseBody(body, response.headers.get('content-type') || undefined),
      isOk: response.ok,
      status: response.status,
      statusText: response.statusText,
    }))
  }

  private parseBody(body: string, contentType?: string): Object | string {
    if (!contentType)
      return body

    if (contentType.includes('application/json'))
      return JSON.parse(body) as Object

    return body
  }

  private parseResponse<T>(raw: RawResponse): DataResponse<T> {
    if (raw.isOk)
      return new DataResponse<T>(raw.body as T, undefined)

    return new DataResponse<T>(undefined as unknown as T, raw.body.toString() || raw.statusText)
  }

  private parseFailure<T>(reason: Stringifiable): DataResponse<T> {
    return new DataResponse<T>(undefined as unknown as T, reason.toString())
  }
}

type RawResponse = {
  isOk: boolean
  status: number
  statusText: string
  body: Stringifiable
}
