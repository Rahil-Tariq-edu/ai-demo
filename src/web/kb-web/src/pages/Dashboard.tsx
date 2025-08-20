import { useEffect, useState } from 'react'
import { api } from '../api/client'
import UploadBox from '../components/UploadBox'

type Doc = { id: string, title: string, sourceType: number, status: number, createdAt: string }

export default function Dashboard() {
  const [docs, setDocs] = useState<Doc[]>([])
  const [textTitle, setTextTitle] = useState('Quick FAQ')
  const [textBody, setTextBody] = useState('Q: What is this app? A: A Knowledge Base Chatbot MVP.')
  const [urlTitle, setUrlTitle] = useState('Homepage')
  const [url, setUrl] = useState('https://example.com')

  const load = async () => {
    const { data } = await api.get('/api/docs')
    setDocs(data)
  }
  useEffect(() => { load() }, [])

  const addText = async () => { await api.post('/api/docs/text', { title: textTitle, text: textBody }); load() }
  const addUrl = async () => { await api.post('/api/docs/url', { title: urlTitle, url }); load() }

  return (
    <div className="p-6 max-w-5xl mx-auto flex flex-col gap-6">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <UploadBox onUploaded={load} />
        <div className="border p-3 rounded flex flex-col gap-2">
          <div className="text-sm font-semibold">Add quick text</div>
          <input className="border p-2" placeholder="Title" value={textTitle} onChange={e=>setTextTitle(e.target.value)} />
          <textarea className="border p-2 h-28" value={textBody} onChange={e=>setTextBody(e.target.value)} />
          <button className="bg-black text-white px-3 py-1" onClick={addText}>Add</button>
        </div>
        <div className="border p-3 rounded flex flex-col gap-2">
          <div className="text-sm font-semibold">Add from URL</div>
          <input className="border p-2" placeholder="Title" value={urlTitle} onChange={e=>setUrlTitle(e.target.value)} />
          <input className="border p-2" placeholder="https://..." value={url} onChange={e=>setUrl(e.target.value)} />
          <button className="bg-black text-white px-3 py-1" onClick={addUrl}>Add</button>
        </div>
      </div>

      <div>
        <div className="text-lg font-semibold mb-2">Documents</div>
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left border-b"><th className="py-2">Title</th><th>Source</th><th>Status</th><th>Created</th></tr>
          </thead>
          <tbody>
            {docs.map(d => (
              <tr key={d.id} className="border-b"><td className="py-2">{d.title}</td><td>{['Upload','Text','Url'][d.sourceType]}</td><td>{['Uploaded','Processed'][d.status]}</td><td>{new Date(d.createdAt).toLocaleString()}</td></tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

