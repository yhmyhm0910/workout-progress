import { DOCUMENT, NgStyle, CommonModule } from '@angular/common';
import { Component, DestroyRef, effect, inject, OnInit, Renderer2, signal, WritableSignal } from '@angular/core';
import { FormControl, FormsModule, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ChartOptions, ChartData } from 'chart.js';
import {
  AvatarComponent,
  ButtonCloseDirective,
  ButtonDirective,
  ButtonGroupComponent,
  CardBodyComponent,
  CardComponent,
  CardFooterComponent,
  CardHeaderComponent,
  ColComponent,
  FormCheckLabelDirective,
  FormDirective,
  FormLabelDirective,
  FormControlDirective,
  FormCheckComponent,
  FormCheckInputDirective,
  GutterDirective,
  ProgressBarDirective,
  ProgressComponent,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  ModalTitleDirective,
  ModalToggleDirective,
  RowComponent,
  TableDirective,
  TextColorDirective
} from '@coreui/angular';
import { ChartjsComponent } from '@coreui/angular-chartjs';
import { IconDirective } from '@coreui/icons-angular';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { WidgetsBrandComponent } from '../widgets/widgets-brand/widgets-brand.component';
import { WidgetsDropdownComponent } from '../widgets/widgets-dropdown/widgets-dropdown.component';
import { DashboardChartsData, IChartProps } from './dashboard-charts-data';
import { exercises_name_by_body_part } from './dashboard-exercisesName.data'

import { HttpClient } from '@angular/common/http';

import { environment } from '../../../environment/environment';


@Component({
  templateUrl: 'dashboard.component.html',
  styleUrls: ['dashboard.component.scss'],
  standalone: true,
  imports: [CommonModule,WidgetsDropdownComponent, TextColorDirective, CardComponent, CardBodyComponent, RowComponent, ColComponent, ButtonDirective, IconDirective, ReactiveFormsModule, ButtonGroupComponent, FormCheckLabelDirective, ChartjsComponent, NgStyle, CardFooterComponent, GutterDirective, ProgressBarDirective, ProgressComponent, WidgetsBrandComponent, CardHeaderComponent, TableDirective, AvatarComponent, FormDirective, FormLabelDirective, FormControlDirective,   FormCheckComponent, FormCheckInputDirective, FormsModule, ModalComponent, ModalHeaderComponent, ModalTitleDirective, ModalBodyComponent, ModalFooterComponent, ModalToggleDirective, ButtonCloseDirective, NgbModule]
})

export class DashboardComponent implements OnInit {

  readonly #destroyRef: DestroyRef = inject(DestroyRef);
  readonly #document: Document = inject(DOCUMENT);
  readonly #renderer: Renderer2 = inject(Renderer2);
  readonly #chartsData: DashboardChartsData = inject(DashboardChartsData);
  readonly exercises_name_by_body_part: string[][] = exercises_name_by_body_part;
  readonly backend_apiUrl: string = environment.backend_apiUrl;
  isClicked = false;
  addExercisesClicked = false;
  today = { year: new Date().getFullYear(), month: new Date().getMonth() + 1, day: new Date().getDate() };
  cardCount = 1; // Track the number of cards

  userId = 0;
  selectedInputtingExerciseName = "";
  inputtedOneRepMax = 0;
  constructor(private httpClient: HttpClient) {}

  ngOnInit(): void {
    this.initCharts();
    this.updateChartOnColorModeChange();
  }

  // Method to fetch products
  getProducts() {
    this.httpClient.get(`${this.backend_apiUrl}/api/Database/Select`, { withCredentials: true })
    .subscribe(
      (res) => {
        console.log(res); // Handle the response
      },
      (error) => {
        console.error('Error fetching products', error); // Handle errors
      }
    );
  }



  addExerciseButton() {
    this.addExercisesClicked = !this.addExercisesClicked;
  }

  changeInputtingExercise(exercise: string) {
    this.selectedInputtingExerciseName = exercise;
  }

  addRecordButton() {
      const params = {
        userId: this.userId,
        exerciseName: this.selectedInputtingExerciseName,
        oneRepMax: this.inputtedOneRepMax,
        unit: "lbs"
      };
    // this.httpClient.post(`${this.backend_apiUrl}/api/Database/InsertRecord`, params).subscribe(
    //   (res) => {
    //     console.log(res); // Handle the re100sponse
    //   },
    //   (error) => {
    //     console.error('Error fetching products', error); // Handle errors
    //   }
    // );
  }

  selectedBodyPart: string | null = null;

  // Function to select only one checkbox at a time
  selectOnly(bodyPart: string) {
    // console.log("this.selectedBodyPart: " + this.selectedBodyPart);
    // console.log("bodyPart: " + bodyPart);
    // If the clicked body part is already selected, deselect it
    if (this.selectedBodyPart === bodyPart) {
      this.selectedBodyPart = null; // uncheck when clicked again
    } else {
      this.selectedBodyPart = bodyPart;

    }
  }

  hhjj() {
    this.getProducts();
    this.isClicked = !this.isClicked;
    this.cardCount++; // Increment the card count
  }



  months_experienced = [...Array(12).keys()]

  options = {
    scales: {
      x: {
        title: {
          display: true,
          text: 'Months Experienced' // Label for the x-axis
        }
      },
      y: {
        title: {
          display: true,
          text: 'Pound' // Label for the y-axis
        }
      }
    },
    maintainAspectRatio: false
  };

  chartLineData: ChartData = {
    labels: this.months_experienced,
    datasets: [
      {
        label: 'Common Standard',
        backgroundColor: 'rgba(220, 220, 220, 0.2)',
        borderColor: 'rgba(220, 220, 220, 1)',
        pointBackgroundColor: 'rgba(220, 220, 220, 1)',
        pointBorderColor: '#fff',
        data: [this.randomData, this.randomData, this.randomData, this.randomData, this.randomData, this.randomData, this.randomData]
      },
      {
        label: 'My Record',
        backgroundColor: 'rgba(151, 187, 205, 0.2)',
        borderColor: 'rgba(151, 187, 205, 1)',
        pointBackgroundColor: 'rgba(151, 187, 205, 1)',
        pointBorderColor: '#fff',
        data: [this.randomData, this.randomData, this.randomData, this.randomData, this.randomData, this.randomData, this.randomData]
      }
    ]
  };

  get randomData() {
    return Math.round(Math.random() * 100);
  }

  public mainChart: IChartProps = { type: 'line' };
  public mainChartRef: WritableSignal<any> = signal(undefined);
  public chart: Array<IChartProps> = [];
  public trafficRadioGroup = new FormGroup({
    trafficRadio: new FormControl('Month')
  });


  initCharts(): void {
    this.mainChart = this.#chartsData.mainChart;
  }

  setTrafficPeriod(value: string): void {
    this.trafficRadioGroup.setValue({ trafficRadio: value });
    this.#chartsData.initMainChart(value);
    this.initCharts();
  }

  handleChartRef($chartRef: any) {
    if ($chartRef) {
      this.mainChartRef.set($chartRef);
    }
  }

  updateChartOnColorModeChange() {
    const unListen = this.#renderer.listen(this.#document.documentElement, 'ColorSchemeChange', () => {
      this.setChartStyles();
    });

    this.#destroyRef.onDestroy(() => {
      unListen();
    });
  }

  setChartStyles() {
    if (this.mainChartRef()) {
      setTimeout(() => {
        const options: ChartOptions = { ...this.mainChart.options };
        const scales = this.#chartsData.getScales();
        this.mainChartRef().options.scales = { ...options.scales, ...scales };
        this.mainChartRef().update();
      });
    }
  }
}
