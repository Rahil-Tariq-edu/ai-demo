import { useState } from 'react'
import { api } from '../api/client'

export default function UploadBox({ onUploaded }: { onUploaded: () => void }) {
  const [file, setFile] = useState<File | null>(null)
  const [title, setTitle] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  const upload = async () => {
    if (!file) return
    setBusy(true); setError('')
    try {
      const form = new FormData()
      form.append('file', file)
      form.append('title', title || file.name)
      await api.post('/api/docs/upload', form, { headers: { 'Content-Type': 'multipart/form-data' } })
      onUploaded()
      setFile(null); setTitle('')
    } catch (e:any) {
      setError(e?.response?.data?.message || 'Upload failed')
    } finally { setBusy(false) }
  }

  return (
    <div className="border p-3 rounded flex flex-col gap-2">
      <div className="text-sm font-semibold">Upload a file</div>
      <input className="border p-2" placeholder="Title (optional)" value={title} onChange={e=>setTitle(e.target.value)} />
      <input type="file" onChange={e=>setFile(e.target.files?.[0] || null)} />
      <div className="flex gap-2">
        <button className="bg-black text-white px-3 py-1 disabled:opacity-50" disabled={!file || busy} onClick={upload}>Upload</button>
        {error && <div className="text-red-600 text-sm">{error}</div>}
      </div>
    </div>
  )
}

