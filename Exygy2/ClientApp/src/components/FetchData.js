import React, { Component } from 'react';
import { Row } from 'reactstrap';

export class FetchData extends Component {
  static displayName = FetchData.name;

  constructor(props) {
    super(props);
      this.state = {
          exygyProperties: [],
          loading: true,
          recordsPerPage: 10,
          pageNumber: 1,
          validPages: [],
          nameFilter: "",
          minOccupancy: 1,
          maxOccupancy: 13,
          validAmenities: [],
          selectedAmenities: []
      };

      this.handleRecordsPerPageChange = this.handleRecordsPerPageChange.bind(this);
  }

   componentDidMount() {
      this.populatePropertiesData(this.state);
    }

    handleRecordsPerPageChange(event) {
        this.setState({ loading: true, pageNumber: 1, recordsPerPage: event.target.value });
        this.populatePropertiesData({
            ...this.state,
            recordsPerPage: event.target.value
        });
    }

    handlePageChange(event) {
        this.setState({ loading: true, pageNumber: event.target.value });
        this.populatePropertiesData({
            ...this.state,
            pageNumber: event.target.value
        });
    }

    filterDelayHandle = null;
    handlePropertyNameChange(event) {
        this.setState({ nameFilter: event.target.value });
        if (this.filterDelayHandle != null)
            clearTimeout(this.filterDelayHandle);
        const targetValue = event.target.value;
        this.filterDelayHandle = setTimeout(() => this.populatePropertiesData({
            ...this.state,
            nameFilter: targetValue
        }), 500);
    }

    handleMinOccupancyChange(event) {
        this.setState({ minOccupancy: event.target.value });
        this.populatePropertiesData({
            ...this.state,
            minOccupancy: event.target.value
        });
    }

    handleMaxOccupancyChange(event) {
        this.setState({ maxOccupancy: event.target.value });
        this.populatePropertiesData({
            ...this.state,
            maxOccupancy: event.target.value
        });
    }

    handleAmenityChange(event, amenity) {
        let selectedAmenities;
        if (event.target.checked) {
            this.state.selectedAmenities.push(amenity)
            selectedAmenities = [...this.state.selectedAmenities];
        }
        else
            selectedAmenities = this.state.selectedAmenities.filter(selectedAmenity => selectedAmenity != amenity);
        this.setState({ selectedAmenities: selectedAmenities });
        this.populatePropertiesData({
            ...this.state,
            selectedAmenities: selectedAmenities
        });
    }

  renderPropertyRows(exygyProperties) {
    return exygyProperties.map(exygyProperty =>
        <div key={ exygyProperty.id } className="row property">
            <div className="col-12 col-md-6 property-image">
                <h4>{exygyProperty.name}</h4>
                <img className="img-fluid" alt={exygyProperty.name} src={exygyProperty.pictureUrl} />
            </div>
            <div className="col-12 col-md-6 unit-info">
                <table className="table">
                    <thead>
                        <tr>
                            <th>
                                Unit Type
                            </th>
                            <th>
                                Average Square Footage
                            </th>
                            <th>
                                Range
                            </th>
                        </tr>
                    </thead>
                    <tbody>
                    {exygyProperty.unitsTypes.map(exygyPropertyUnitType =>
                        <tr key={exygyPropertyUnitType.unitType}>
                            <td>
                                {exygyPropertyUnitType.unitType}
                            </td>
                            <td>
                                {exygyPropertyUnitType.avgUnitSqft}
                            </td>
                            <td>
                                {exygyPropertyUnitType.minMinOccupancy} - {exygyPropertyUnitType.maxMaxOccupancy}
                            </td>
                        </tr>)
                        }
                    </tbody>
                </table>
            </div>
        </div>);
    }

    renderControl() {
        return <div className="exygyApp">
            <div className="row justify-content-between controls">
                <div className="col-12 col-md-9 filters">
                    <label>
                        Filter: <input  onChange={(event => this.handlePropertyNameChange(event))} placeholder="property name" title="Start typing a property name here to filter the displayed properties." type="text" defaultValue={this.state.nameFilter } />
                    </label> <label>
                        Min Occupancy: <input type="number" min="1" max="13" onChange={(event) => this.handleMinOccupancyChange(event)} value={this.state.minOccupancy} />
                    </label> <label>
                        Max Occupancy: <input type="number" min="1" max="13" onChange={(event) => this.handleMaxOccupancyChange(event)} value={this.state.maxOccupancy} />
                    </label>
                    <div>
                        {this.state.validAmenities.map((validAmenity) => <label key={validAmenity}>{validAmenity}:&nbsp;<input type="checkbox" name="amenity" checked={this.state.selectedAmenities.includes(validAmenity)} onChange={(event) => this.handleAmenityChange(event, validAmenity)} />&nbsp;</label>)}
                    </div>
                </div>
                <div className="col-12 col-md-3 paging">
                    <label>
                        Current Page: <select value={this.state.pageNumber} onChange={(event) => this.handlePageChange(event) }>
                            {this.state.validPages.map(pageNumber => <option key={pageNumber}>{pageNumber}</option>)}
                        </select>
                    </label> <label>
                        Records Per Page: <select value={this.state.recordsPerPage} onChange={(event) => this.handleRecordsPerPageChange(event) }>
                            <option>10</option>
                            <option>20</option>
                            <option>30</option>
                            <option>40</option>
                            <option>50</option>
                        </select>
                    </label>
                </div>
            </div>
            { this.renderPropertyRows(this.state.exygyProperties) }
        </div>;
    }

  render() {
      let contents = this.state.loading
     ? <p><em>Loading...</em></p>
          : this.renderControl();
    return (
      <div>
        {contents}
      </div>
    );
  }

    async populatePropertiesData(state) {
      const selectedAmenities = state.selectedAmenities.map(amenity => `&amenities=${encodeURIComponent(amenity)}`).join("");
      const response = await fetch(
          `/ExygyProperties?recordsPerPage=${state.recordsPerPage}&page=${state.pageNumber}&propertyName=${encodeURIComponent(state.nameFilter)}&minOccupancy=${state.minOccupancy}&maxOccupancy=${state.maxOccupancy}${selectedAmenities}`,
          {
              method: 'GET'
          }
        ); 
      const exygyPage = await response.json();
        this.setState({ exygyProperties: exygyPage.properties, loading: false, validPages: exygyPage.validPages, validAmenities: exygyPage.validAmenities });
  }
}
