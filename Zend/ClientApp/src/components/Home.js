import React, { Component } from 'react';
import Card from 'reactstrap/lib/Card';
import CardBody from 'reactstrap/lib/CardBody';
import { UploadedFile } from '../models/UploadedFile';
import { toLocaleTimestamp } from '../utils/dateTime';
import "./Home.css";

/* Wow.  React without TypeScript is really not fun.  */

export class Home extends Component {
  static displayName = Home.name;

  hiddenFileInput = React.createRef();
  searchInput = React.createRef();

  constructor() {
    super();

    const uploadsString = window.localStorage.getItem("uploads");
    const existingUploads = (uploadsString && JSON.parse(uploadsString)) || [];

    this.state = {
      uploads: existingUploads,
      filteredUploads: existingUploads,
      clipboardText: ""
    }
  }

  componentDidMount() {
    window.addEventListener("dragover", ev => {
      ev.preventDefault();
      ev.dataTransfer.dropEffect = "copy";
    });

    window.addEventListener("drop", ev => {
      ev.preventDefault();

      if (ev.dataTransfer.files.length < 1) {
        return;
      }
      this.uploadFiles(ev.dataTransfer.files);
    });

    this.hiddenFileInput.current.addEventListener("change", ev => {
      this.uploadFiles(ev.target.files);
    });
  }

  componentWillUnmount() {
    window.removeEventListener("dragover");
    window.removeEventListener("drop");
  }

  filterUploads = () => {
    if (!this.state.uploads || this.state.uploads.length === 0) {
      this.setState({
        filteredUploads: []
      })
    }

    if (!this.searchInput.current || !this.searchInput.current.value) {
      this.setState({
        filteredUploads: this.state.uploads
      })
    }

    const searchTerm = this.searchInput.current.value.toLowerCase();

    const filtered = this.state.uploads.filter(x =>
      x.fileName.toLowerCase().includes(searchTerm));

    this.setState({
      filteredUploads: filtered
    })
  }


  openFilePrompt = () => {
    if (this.hiddenFileInput?.current) {
      this.hiddenFileInput.current.click();
    }
  }

  uploadFiles = (fileList) => {
    if ((fileList || []).length === 0) {
      return;
    }

    for (let i = 0; i < fileList?.length; i++) {
      const file = fileList[i];
      const upload = new UploadedFile(file.name, file.size, new Date());
      this.state.uploads.unshift(upload);

      const fd = new FormData();
      fd.append('file', file);
      const xhr = new XMLHttpRequest();
      xhr.open('POST', `${window.location.origin}/api/file`, true);

      xhr.addEventListener("load", () => {
        if (xhr.status === 200) {
          let savedFile = JSON.parse(xhr.responseText);
          upload.id = savedFile.id;
          upload.url = `${window.location.origin}/api/file/${upload.id}`;

          this.setState({
            uploads: this.state.uploads,
          });

          this.filterUploads();

          window.localStorage.setItem("uploads", JSON.stringify(this.state.uploads));
        }
        else {
          alert(`File upload failed. Status code: ${xhr.status}`);
          this.removeUpload(upload.id);
        }
      });

      xhr.upload.addEventListener("progress", (e) => {
        const newPercent = e.loaded / e.total;

        upload.percentUploaded = newPercent;

        this.setState({
          uploads: this.state.uploads
        });

        this.filterUploads();
      });
      xhr.send(fd);
    }
  }

  removeAllUploads = () => {
    if (!this.state.uploads || this.state.uploads.length == 0) {
      return;
    }

    this.state.uploads.forEach((x, index) => {
      window.setTimeout(() => {
        this.removeUpload(x.id);
      }, index * 100)
    });
  }

  removeUpload = (id) => {
    const index = this.state.uploads.findIndex(x => x.id === id);
    this.state.uploads.splice(index, 1);
    this.setState({
      uploads: this.state.uploads
    });

    window.localStorage.setItem("uploads", JSON.stringify(this.state.uploads));

    this.filterUploads();

    fetch(`${window.location.origin}/api/file/${id}`, {
      method: 'delete'
    })
  }

  setClipboardText = (text) => {
    window.navigator.clipboard.writeText(text);
    this.setState({
      clipboardText: text
    })
  }

  render() {
    return (
      <div className="text-center">
        <div className="d-inline-block">

          <div className="dropzone" onClick={this.openFilePrompt}>
            <Card className="dropcard">
              <CardBody>
                <div className="droptext">
                  Drop files or click here.
              </div>
              </CardBody>
            </Card>
          </div>

          <div className="text-left">
            <h5 className="text-left mt-5">Uploads</h5>

            <div className="searchbox mt-2">
              <label>Search</label>
              <br />
              <input ref={this.searchInput} type="text" className="form-control" onInput={this.filterUploads}></input>
            </div>
          </div>

          <div className="text-right mt-2">
            <button className="btn btn-sm btn-danger" onClick={this.removeAllUploads}>
              Delete All</button>
          </div>

          <div className="uploads-frame text-left mt-3">
            {this.renderUploads()}
          </div>

        </div>

        <input ref={this.hiddenFileInput} type="file" multiple hidden></input>
      </div>
    );
  }

  renderUploads() {
    if (!this.state.filteredUploads || this.state.filteredUploads.length === 0) {
      return "No uploads found.";
    }

    return this.state.filteredUploads.map(x => {

      return (
        <Card key={x.id} className="small mb-3">
          <CardBody>
            <div className="upload-body">
              {x.percentUploaded < 1 && (
                <h6 className="font-weight-bold">Upload:</h6>
              )}

              {x.percentUploaded < 1 && (
                <div>
                  <progress className="w-100" value={x.percentUploaded} max={1}></progress>
                </div>
              )}

              <h6 className="font-weight-bold">Name:</h6>
              <div>{x.fileName}</div>

              <h6 className="font-weight-bold">Size:</h6>
              <div>{Number(x.fileSize).toLocaleString()}</div>

              <h6 className="font-weight-bold">At:</h6>
              <div>{toLocaleTimestamp(x.uploadedAt)}</div>

              <h6 className="font-weight-bold">Link:</h6>
              <div><a href={x.url} target="_blank" rel="noopener noreferrer">{x.url}</a></div>

              <div className="text-right mt-2" style={{ gridColumn: 'span 2' }}>

                {this.renderCopyButton(x)}
                <button className="btn btn-sm btn-danger" onClick={() => this.removeUpload(x.id)}>
                  Delete
                </button>
              </div>
            </div>
          </CardBody>
        </Card>
      )
    })
  }

  renderCopyButton(upload) {
    if (this.state.clipboardText == upload.url) {
      return (
        <button className="btn btn-sm btn-success mr-2" onClick={() => this.setClipboardText(upload.url)}>
          Copied
        </button>
      )
    }
    else {
      return (
        <button className="btn btn-sm btn-secondary mr-2" onClick={() => this.setClipboardText(upload.url)}>
          Copy
        </button>
      )
    }
  }
}
